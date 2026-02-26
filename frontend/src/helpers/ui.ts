import { CardBack } from '@/components';
import { h, nextTick, render } from 'vue';

export interface AnimateCardOptions {
  fromEl: HTMLSnapshot;
  toEl: HTMLElement;
  animatedEl: HTMLElement;
  duration?: number;
  flipCard?: 'face-up' | 'face-down';
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
  fromEl,
  toEl,
  animatedEl,
  duration = 600,
  flipCard,
}: AnimateCardOptions): Promise<void> {
  toEl.style.visibility = 'hidden';

  const fromRect = fromEl.boundingClientRect;
  const toRect = toEl.getBoundingClientRect();

  const { scaleX, scaleY, rotate: fromRotation } = fromEl;

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

  const toRotation = toEl.style.rotate || '0deg';

  const startingStyles = {
    position: 'absolute',
    left: `${fromCenterX - width / 2}px`,
    top: `${fromCenterY - height / 2}px`,
    width: `${width}px`,
    height: `${height}px`,
    zIndex: '999',
  };

  Object.assign(animatedEl.style, startingStyles);

  let displays: [string, string], container: HTMLDivElement;

  if (flipCard) {
    container = document.createElement('div');
    animatedEl.parentElement?.appendChild(container);
    render(h(CardBack), container);

    if (flipCard === 'face-down') {
      displays = ['', 'none'];
    } else {
      displays = ['none', ''];
    }

    animatedEl.style.display = displays[0];
    Object.assign(container.style, {
      ...startingStyles,
      display: displays[1],
    });

    await nextTick();

    setTimeout(() => {
      animatedEl.style.display = displays[1];
      container.style.display = displays[0];
    }, duration / 2);
  }

  const animation = animatedEl.animate(
    [
      {
        transform: `translate(0px, 0px) rotate(${fromRotation}) ${flipCard === 'face-down' ? 'rotateY(0)' : flipCard === 'face-up' ? 'rotateY(180deg)' : ''} scale(1, 1)`,
      },
      {
        transform: `translate(${dx}px, ${dy}px) rotate(${toRotation}) ${flipCard === 'face-down' ? 'rotateY(180deg)' : flipCard === 'face-up' ? 'rotateY(0)' : ''} scale(${1 / scaleX}, ${1 / scaleY})`,
      },
    ],
    { duration, easing: 'linear', fill: 'forwards' }
  );

  let animation2;
  if (flipCard) {
    animation2 = container!.animate(
      [
        {
          transform: `translate(0px, 0px) rotate(${fromRotation}) ${flipCard === 'face-down' ? 'rotateY(180deg)' : flipCard === 'face-up' ? 'rotateY(0)' : ''} scale(1, 1)`,
        },
        {
          transform: `translate(${dx}px, ${dy}px) rotate(${toRotation}) ${flipCard === 'face-down' ? 'rotateY(0)' : flipCard === 'face-up' ? 'rotateY(180deg)' : ''} scale(${1 / scaleX}, ${1 / scaleY})`,
        },
      ],
      { duration, easing: 'linear', fill: 'forwards' }
    );
  }

  await animation.finished;
  if (flipCard) {
    await animation2!.finished;
    container!.style.display = 'none';
  }
  toEl.style.removeProperty('visibility');
}
