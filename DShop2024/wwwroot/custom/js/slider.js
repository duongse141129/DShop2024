const slider = document.getElementById('sliderpost');
const totalCards = slider.children.length;
let currentIndex = 0;

function goToSlide(index) {
    currentIndex = index;
    slider.style.transform = `translateX(-${index * 100}%)`;
    updatePagination();
}

function updatePagination() {
    const pagination = document.getElementById('pagination');
    pagination.innerHTML = '';
    for (let i = 0; i < totalCards; i++) {
        const dot = document.createElement('span');
        dot.className = 'dot' + (i === currentIndex ? ' active' : '');
        dot.onclick = () => goToSlide(i);
        pagination.appendChild(dot);
    }
}

function prevSlide() {
    if (currentIndex > 0) {
        goToSlide(currentIndex - 1);
    }
}

function nextSlide() {
    if (currentIndex < totalCards - 1) {
        goToSlide(currentIndex + 1);
    }
}
function startAutoSlide() {
    autoSlideInterval = setInterval(nextSlide, 4000); 
}

function stopAutoSlide() {
    clearInterval(autoSlideInterval);
}

startAutoSlide();
updatePagination();