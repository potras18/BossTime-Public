var nOffset = 0;

function ShowCurrent(offset = 0) {
    nOffset = offset;
    const d = new Date();
    
    let hour = d.getUTCHours();
    let bmins = d.getUTCMinutes();
    var timeslot = document.getElementById("dlMain_lblHour_" + hour);
    var furyslot = document.getElementById("dlMain_lblFuryHour_" + hour);
    var elemmins = document.getElementById("hfMins");
    var mins = parseInt(elemmins.value);

    if (bmins >= 0) {
        if (furyslot) {
            furyslot.innerText = "Fury:SPAWNED";
        }
    }

    if (timeslot) {
        if (bmins >= mins) {
            timeslot.parentElement.classList.add("TimeSelExp");
            timeslot.innerText = "Boss:SPAWNED";

        } else {
            timeslot.parentElement.classList.add("TimeSel");
        }
    }

    FakeClick(1);
}


function ShowS() {
    const d = new Date();
   
    let hour = d.getUTCHours();
    let bmins = d.getUTCMinutes();
    var timeslot = document.getElementById("dlMain_lblHour_" + hour);
    var elemmins = document.getElementById("hfMins");
    var mins = parseInt(elemmins.value);

    try {
        var parelem = timeslot.parentElement;
        if (parelem) {
            parelem.classList.add("TimeSel");
        } else {
            timeslot = document.getElementById("dlMain_lblHour_" + (hour + 1));
            parelem = timeslot.parentElement;
            parelem.classList.add("TimeSel");
        }
        FakeClick(1);

    } catch (e) { }
}


function StartTimer(offset=0) {
    setInterval(function () { 

        var elem = document.getElementById("lblsTime");
        var elem2 = document.getElementById("lblbTime");
        var elemmins = document.getElementById("hfMins");
        if (elem) {
            
            const d = new Date();
            
            var hour = d.getUTCHours();
            var min = d.getUTCMinutes();
            var sec = d.getUTCSeconds();

            elem.innerHTML = "Current Server Time - " + hour.toString().padStart(2, '0') + ":" + min.toString().padStart(2, '0') + ":" + sec.toString().padStart(2, '0');
            //elem2.innerHTML = "Current Boss Time - XX:" + min.toString().padStart(2, '0') (;
        }


    }, 500);
}

function FuryClick() {
    playAudio("Resources/fury_5.mp3");
}

function BossClick() {
    playAudio("Resources/boss_5.mp3")
}

function ScrlClick() {
    const d = new Date();
    let hour = d.getUTCHours();
    var timeslot = document.getElementById("dlMain_lblHour_" + hour);
    if (timeslot) {
        const y = timeslot.getBoundingClientRect().top + window.scrollY;
        window.scroll({
            top: y - 100,
            behavior: 'smooth'
        });
    }
}

function playAudio(track) {
    var aud = new Audio(track);
    var prms = aud.play();
    if (prms != undefined) {
        var dism = getCookie("audDismiss");
        if (dism != "1") {
            var elem = document.getElementById("pnlAudio");
            elem.style = "display:block";
        }
    }
}



function FakeClick(mode) {
    var func = "";
    switch (mode) {
        case 1:
            func = "btnScrl";
            break;
        case 2:
            func = "btnFury";
            break;
        case 3:
            func = "btnBoss";
            break;
        case 4:
            func = "btnSnt";
            break;
    }

    var elem = document.getElementById(func);
    elem.click();


}

function SetMinMax(type, level) {
    if (type == 1) {
        setCookie("low", level,365);
    } else if (type == 2) {
        setCookie("high", level,365);
    }

    var up = '<%=upMain.ClientID%>';

    if (up) {
        __doPostBack(up, '');
    }
}

function GetMinMax() {
    
    var tblow = document.getElementById("lvlFrom");
    var tbhigh = document.getElementById("lvlTo");

    var lowactual = parseInt(getCookie("low"));
    var highactual = parseInt(getCookie("high"));


    var finalHigh = highactual;
    var finalLow = lowactual;

    if (lowactual > highactual) {
        finalHigh = lowactual;
        setCookie("high", finalHigh,365);
        finalLow = highactual;
        setCookie("low", finalLow,365);
    }

    


    if (lowactual) {
        tblow.value = finalLow;
    }

    if (highactual) {
        tbhigh.value = finalHigh;
    }


}

function addSCookie(elem) {
    var val = elem.checked;
    
    setCookie("showsyler", val, 365);

    var up = '<%=upMain.ClientID%>';

    if (up) {
        __doPostBack(up, '');
    }
}

function HideSpeakerPanel() {

    var elem = document.getElementById("pnlAudio");
    elem.style = "display:none";
    window.cookieStore.set("audDismiss", "1");
}

function getCookie(name) {
    let cookie = {};
    document.cookie.split(';').forEach(function (el) {
        let split = el.split('=');
        cookie[split[0].trim()] = split.slice(1).join("=");
    })
    return cookie[name];
}

function setCookie(cname, cvalue, exdays) {
    const d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}