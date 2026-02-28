import { CardBack } from '@/components';
import { h, render } from 'vue';

export interface AnimateCardOptions {
  fromEl: HTMLSnapshot;
  toEl: HTMLElement;
  animatedEl: HTMLElement;
  duration?: number;
  easing?: string;
  flipCard?: 'face-up' | 'face-down';
  zIndex?: number;
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

function buildTransform(
  dx: number,
  dy: number,
  rotation: string,
  rotateY: string,
  scaleX = 1,
  scaleY = 1
) {
  return `translate(${dx}px, ${dy}px) rotate(${rotation}) perspective(600px) ${rotateY} scale(${scaleX}, ${scaleY})`;
}

export async function animateCardMove({
  fromEl,
  toEl,
  animatedEl,
  duration = 600,
  easing = 'linear',
  flipCard,
  zIndex = 999,
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
    zIndex: String(zIndex),
  };

  if (flipCard) {
    // A preserve-3d wrapper travels the full path + flips rotateY(0 → 180deg).
    // Both faces sit inside it with backface-visibility: hidden, so CSS handles
    // the face swap automatically at rotateY(90deg) — no setTimeout needed,
    // and any easing function works correctly.
    const wrapper = document.createElement('div');
    wrapper.id = 'wrapper';
    Object.assign(wrapper.style, {
      ...startingStyles,
      transformStyle: 'preserve-3d',
    });

    const faceStyles: Partial<CSSStyleDeclaration> = {
      position: 'absolute',
      top: '0',
      left: '0',
      width: '100%',
      height: '100%',
      backfaceVisibility: 'hidden',
    };

    const cardBackFace = document.createElement('div');
    render(h(CardBack), cardBackFace);
    Object.assign(cardBackFace.style, faceStyles);

    // Wrap animatedEl in a plain div so Vue reactive re-renders can't touch our
    // 3D transform offset. The face that needs to START hidden gets the 180deg
    // offset (backface-visibility: hidden works reliably for elements that are
    // already back-facing at mount time). That face is also appended LAST so it
    // paints on top once it becomes front-facing — the other face stays in the
    // background even if backface-visibility doesn't dynamically hide it.
    //
    // face-down: card face visible first → CardBack reveals second (appended last)
    // face-up:   CardBack visible first  → card face reveals second (appended last)
    const originalParent = animatedEl.parentElement!;

    const animatedElFace = document.createElement('div');
    Object.assign(animatedElFace.style, faceStyles);
    Object.assign(animatedEl.style, {
      position: 'absolute',
      top: '0',
      left: '0',
      width: '100%',
      height: '100%',
    });
    animatedElFace.appendChild(animatedEl);

    originalParent.appendChild(wrapper);

    if (flipCard === 'face-up') {
      animatedElFace.style.transform = 'rotateY(180deg)'; // starts hidden, reveals last
      wrapper.appendChild(cardBackFace); // first (underneath)
      wrapper.appendChild(animatedElFace); // last (on top when revealed)
    } else {
      cardBackFace.style.transform = 'rotateY(180deg)'; // starts hidden, reveals last
      wrapper.appendChild(animatedElFace); // first (underneath)
      wrapper.appendChild(cardBackFace); // last (on top when revealed)
    }

    await wrapper.animate(
      [
        { transform: buildTransform(0, 0, fromRotation, 'rotateY(0deg)') },
        // prettier-ignore
        { transform: buildTransform(dx, dy, toRotation, 'rotateY(180deg)', 1 / scaleX, 1 / scaleY) },
      ],
      { duration, easing, fill: 'forwards' }
    ).finished;

    // Restore animatedEl to its original parent (from inside animatedElFace)
    // before Vue cleans it up, then tear down the wrapper.
    animatedEl.style.display = 'none';
    originalParent.insertBefore(animatedEl, wrapper);
    wrapper.remove();
  } else {
    Object.assign(animatedEl.style, startingStyles);

    await animatedEl.animate(
      [
        { transform: buildTransform(0, 0, fromRotation, '') },
        // prettier-ignore
        { transform: buildTransform(dx, dy, toRotation, '', 1 / scaleX, 1 / scaleY) },
      ],
      { duration, easing, fill: 'forwards' }
    ).finished;
  }

  toEl.style.removeProperty('visibility');
}
