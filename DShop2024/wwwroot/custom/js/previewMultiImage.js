let fileInput = document.getElementById("file-input");
let imageContainer = document.getElementById("previewImages");
let numOfFiles = document.getElementById("num-of-files");

function preview() {
    imageContainer.innerHTML = "";
    numOfFiles.style.display = "";
    numOfFiles.textContent = `${fileInput.files.length} Files Selected`;

    for (i of fileInput.files) {
        let reader = new FileReader();
        let figure = document.createElement("figure");
        let figCap = document.createElement("figcaption");
        figCap.innerText = i.name;
        figure.appendChild(figCap);
        reader.onload = () => {
            let img = document.createElement("img");
            img.setAttribute("src", reader.result);
            img.setAttribute("width", "100px");
            img.setAttribute("height", "120px");
            figure.insertBefore(img, figCap);
        }
        imageContainer.appendChild(figure);
        reader.readAsDataURL(i);
    }
}