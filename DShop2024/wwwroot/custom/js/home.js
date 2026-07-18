

/*home coupon*/

const grid = document.getElementById('couponGrid');

document.querySelectorAll('.coupon-desc').forEach(desc => {
    if (desc.scrollHeight > desc.clientHeight + 1) {
        desc.classList.add('has-more');
    }
});
grid.addEventListener('click', (e) => {
    const desc = e.target.closest('.coupon-desc');
    if (desc) {
        if (!desc.classList.contains('has-more')) return;
        desc.classList.toggle('expanded');
        desc.closest('.coupon').classList.toggle('expanded');
        return;
    }

    const btn = e.target.closest('.code-chip');
    if (!btn) return;
    const code = btn.dataset.code;
    navigator.clipboard?.writeText(code).catch(() => { });

    const icon = btn.querySelector('.chip-icon');
    const text = btn.querySelector('.chip-text');
    const originalText = text.textContent;
    text.textContent = 'Copied';
    btn.classList.add('copied');

    setTimeout(() => {
        text.textContent = originalText;
        btn.classList.remove('copied');
    }, 1400);
});





/*Home sale product*/


const track = document.getElementById('track-sale');


function cardsPerView() {
    const w = window.innerWidth;
    if (w <= 420) return 1;
    if (w <= 620) return 2;
    if (w <= 860) return 3;
    if (w <= 1100) return 4;
    return 5;
}


function cardStep() {
    const card = track.querySelector('.ticket');
    if (!card) return 300;
    const gap = 22;
    return card.getBoundingClientRect().width + gap;
}

document.getElementById('nextBtn').addEventListener('click', () => {
    track.scrollBy({ left: cardStep() * cardsPerView(), behavior: 'smooth' });
});
document.getElementById('prevBtn').addEventListener('click', () => {
    track.scrollBy({ left: -cardStep() * cardsPerView(), behavior: 'smooth' });
});

function refreshButtons() {
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const maxScroll = track.scrollWidth - track.clientWidth - 2;
    prevBtn.disabled = track.scrollLeft <= 0;
    nextBtn.disabled = track.scrollLeft >= maxScroll;
}

track.addEventListener('scroll', refreshButtons);
window.addEventListener('resize', () => { refreshButtons(); });


refreshButtons();




