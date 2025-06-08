"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();


var buttonSend = document.getElementById("sendContactButton")
if (buttonSend != null) {
    buttonSend.disabled = true;
}

//ReceiveMessage
connection.on("SendContact", function (user, message) {

    var linkA = document.createElement("a");
    var divImg = document.createElement("div");
    var divContent = document.createElement("div");
    var avatar = document.createElement("img");
    var spanUserName = document.createElement("span");
    var strongUserName = document.createElement("strong");
    var spanMessage = document.createElement("span");
    var spanTime = document.createElement("span");
    var contentMessage = `${message["subject"]}`;
    if (contentMessage.length > 10) {
        contentMessage = contentMessage.substring(0, 10);
    }

    if (user == message["userName"]) {
        var idListContact = `listContactnotification`;
        var contacs = document.getElementById(idListContact);
        contacs.appendChild(linkA);
        linkA.href = `${message["linkContact"]}`;
        linkA.appendChild(divImg);
        divImg.classList.add("notif-img");
        divImg.appendChild(avatar)
        avatar.src = `${message["pathImage"]}`;

        linkA.appendChild(divContent);
        divContent.classList.add("notif-content");
        divContent.appendChild(spanUserName);
        spanUserName.classList.add("subject");
        spanUserName.appendChild(strongUserName);
        strongUserName.textContent = `${message["userName"]}`;
        divContent.appendChild(spanMessage);
        spanMessage.classList.add("block");
        //spanMessage.textContent = `Subject: ${contentMessage}`;
        spanMessage.textContent = "Subject: " + contentMessage;
        divContent.appendChild(spanTime);
        spanTime.classList.add("time");
        spanTime.textContent = `${message["dateSent"]}`;

        var countNotiContact = document.getElementById(`countContacts`);
        if (countNotiContact.textContent == "") {
            countNotiContact.classList.add("notification");
            countNotiContact.textContent = `1`;
        } else {
            countNotiContact.textContent = parseInt(countNotiContact.textContent) + 1;
        }
    }
});



//create and connect
connection.start().then(function () {
    //document.getElementById("sendMessageButton").disabled = false;
    var bSend = document.getElementById("sendContactButton")
    if (bSend != null) {
        bSend.disabled = false;
    }
}).catch(function (err) {
    return console.error(err.toString());
});

