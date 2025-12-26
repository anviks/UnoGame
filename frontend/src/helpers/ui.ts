import { nextTick, type Ref } from 'vue';

interface AnimateCardOptions {
  fromRect: DOMRect;
  toElement: HTMLElement;
  styleRef: Ref<Record<string, string>>;
  duration?: number;
}

export async function animateCardMove({
  fromRect,
  toElement,
  styleRef,
  duration = 600,
}: AnimateCardOptions): Promise<void> {
  return new Promise(async (resolve, reject) => {
    toElement.style.visibility = 'hidden';
    await nextTick();

    const toRect = toElement.getBoundingClientRect();

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
      transform: `translate(${dx}px, ${dy}px) rotate(${rotation})`,
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
