import * as _ from 'lodash-es';
import { nextTick, type Ref } from 'vue';

interface AnimateCardOptions {
  fromElement: HTMLSnapshot;
  toElement: HTMLElement;
  styleRef: Ref<Record<string, string>>;
  duration?: number;
}

interface HTMLSnapshot {
  boundingClientRect: DOMRect;
  offsetWidth: number;
  offsetHeight: number;
  scaleX: number;
  scaleY: number;
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

    const { scaleX, scaleY } = fromElement;

    const fromCenterX = fromRect.left + fromRect.width / 2;
    const fromCenterY = fromRect.top + fromRect.height / 2;

    const toCenterX = toRect.left + toRect.width / 2;
    const toCenterY = toRect.top + toRect.height / 2;

    const dx = toCenterX - fromCenterX;
    const dy = toCenterY - fromCenterY;

    const rotation = toElement.style.rotate || '0deg';

    styleRef.value = {
      position: 'absolute',
      left: `${fromRect.left}px`,
      top: `${fromRect.top}px`,
      width: `${fromRect.width}px`,
      height: `${fromRect.height}px`,
      right: `${fromRect.right}px`,
      bottom: `${fromRect.bottom}px`,
      transform: `translate(${dx}px, ${dy}px) scale(${1 / scaleX}, ${1 / scaleY}) rotate(${rotation})`,
      transition: `transform ${duration}ms ease`,
      zIndex: '999',
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
