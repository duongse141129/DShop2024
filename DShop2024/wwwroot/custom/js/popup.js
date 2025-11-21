function openModal(url) {
    $.get(url, function (res) {
        $("#mainModal .modal-content").html(res);
        var modal = new bootstrap.Modal(document.getElementById("mainModal"));
        modal.show();
    });
}