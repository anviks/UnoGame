import _ from 'lodash';
import { nextTick, type Ref } from 'vue';

interface AnimateCardOptions {
  fromElement: HTMLSnapshot;
  toElement: HTMLElement;
  styleRef: Ref<Record<string, string>>;
  duration?: number;
}

export interface HTMLSnapshot {
  boundingClientRect: DOMRect;
  offsetWidth: number;
  offsetHeight: number;
  scaleX: number;
  scaleY: number;
  rotate: string;
}

interface Dimensions {
  width: number;
  height: number;
}

/**
 * Calculate true dimensions from bounding rectangle and rotation angle
 * @param boundingWidth - Width of the bounding rectangle
 * @param boundingHeight - Height of the bounding rectangle
 * @param rotationDegrees - Rotation angle in degrees
 * @returns Original width and height before rotation
 */
function getTrueDimensions(
  boundingWidth: number,
  boundingHeight: number,
  rotationDegrees: number
): Dimensions {
  // Convert degrees to radians
  const angle = (rotationDegrees * Math.PI) / 180;

  const cos = Math.abs(Math.cos(angle));
  const sin = Math.abs(Math.sin(angle));

  // Solve the system of equations:
  // boundingWidth = width * cos + height * sin
  // boundingHeight = width * sin + height * cos

  const denominator = cos * cos - sin * sin;

  const width = (boundingWidth * cos - boundingHeight * sin) / denominator;
  const height = (boundingHeight * cos - boundingWidth * sin) / denominator;

  return { width, height };
}

export function getElementSnapshot(el: HTMLElement): HTMLSnapshot {
  const rect = el.getBoundingClientRect();
  const scaleX = rect.width / el.offsetWidth || 1;
  const scaleY = rect.height / el.offsetHeight || 1;

  return {
    boundingClientRect: rect,
    offsetWidth: el.offsetWidth,
    offsetHeight: el.offsetHeight,
    scaleX,
    scaleY,
    rotate: el.style.rotate || '0deg',
  };
}

export async function animateCardMove({
  fromElement,
  toElement,
  styleRef,
  duration = 600,
}: AnimateCardOptions): Promise<void> {
  return new Promise(async (resolve, reject) => {
    toElement.style.visibility = 'hidden';
    await nextTick();

    const fromRect = fromElement.boundingClientRect;
    const toRect = toElement.getBoundingClientRect();

    const { scaleX, scaleY, rotate: fromRotation } = fromElement;

    let { width, height } = fromRect;
    ({ width, height } = getTrueDimensions(
      width,
      height,
      parseInt(fromRotation)
    ));

    const fromCenterX = fromRect.left + fromRect.width / 2;
    const fromCenterY = fromRect.top + fromRect.height / 2;

    const toCenterX = toRect.left + toRect.width / 2;
    const toCenterY = toRect.top + toRect.height / 2;

    const dx = toCenterX - fromCenterX;
    const dy = toCenterY - fromCenterY;

    const toRotation = toElement.style.rotate || '0deg';

    const baseStyles = {
      position: 'absolute',
      left: `${fromCenterX - width / 2}px`,
      top: `${fromCenterY - height / 2}px`,
      width: `${width}px`,
      height: `${height}px`,
      zIndex: '999',
    };

    // Phase 1: render at the starting position with correct rotation (no transition)
    styleRef.value = {
      ...baseStyles,
      transform: `translate(0px, 0px) rotate(${fromRotation}) scale(1, 1)`,
    };

    await nextTick();
    void document.body.offsetHeight; // force reflow

    // Phase 2: transition to end position — matching function list ensures
    // component-wise interpolation (translate stays in screen space)
    styleRef.value = {
      ...baseStyles,
      transform: `translate(${dx}px, ${dy}px) rotate(${toRotation}) scale(${1 / scaleX}, ${1 / scaleY})`,
      transition: `transform ${duration}ms ease`,
    };

    /*
     Setting visibility to hidden and waiting for the next tick before cleaning up seems to be
     one of the few ways to get rid of the transitioning card exactly when the transition ends.
     Without it, the transitioning card sticks around for an additional second or two.
    */
    setTimeout(async () => {
      styleRef.value.visibility = 'hidden';
      await nextTick();
      toElement.style.removeProperty('visibility');
      styleRef.value = {};
      resolve();
    }, duration);
  });
}
