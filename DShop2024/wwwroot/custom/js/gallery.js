
document.addEventListener("DOMContentLoaded", function () {
    const thumbnails = document.querySelectorAll('.thumbnail');
    const featured = document.getElementById('featured');

    thumbnails.forEach(thumb => {
        thumb.addEventListener('mouseover', function () {
            const active = document.querySelector('.thumbnail.active');
            if (active) active.classList.remove('active');
            this.classList.add('active');
            featured.src = this.src;
        });
    });

    // --- left/ right button ---
    const buttonRight = document.getElementById('slideRight');
    const buttonLeft = document.getElementById('slideLeft');
    const slider = document.getElementById('slider');

    if (buttonLeft && buttonRight && slider) {
        buttonLeft.addEventListener('click', () => slider.scrollLeft -= 180);
        buttonRight.addEventListener('click', () => slider.scrollLeft += 180);
    }


    // --- Zoom ---
    const container = document.querySelector('.main-img-container');

    if (container && featured) {
        container.addEventListener('mousemove', function (e) {
            const rect = container.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;


            featured.style.transformOrigin = `${x}px ${y}px`;
            featured.style.transform = "scale(2.2)"; // zoom
        });

        container.addEventListener('mouseleave', function () {
            featured.style.transformOrigin = "center center";
            featured.style.transform = "scale(1)";
        });
    }
});

