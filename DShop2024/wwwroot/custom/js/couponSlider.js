let slider = document.querySelector('.slidercoupon');
let scrollAmount = 0;
const cardWidth = slider.querySelector('.card').offsetWidth + 10; // card + margin

function slideLeftCoupon() {
    scrollAmount = Math.max(scrollAmount - cardWidth * 4, 0);
    slider.style.transform = `translateX(-${scrollAmount}px)`;
}

function slideRightCoupon() {
    const maxScroll = slider.scrollWidth - slider.parentElement.offsetWidth;
    scrollAmount = Math.min(scrollAmount + cardWidth * 4, maxScroll);
    slider.style.transform = `translateX(-${scrollAmount}px)`;
}
