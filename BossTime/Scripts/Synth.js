function SynthSpeak() {


    try {

        var txt = speechText;
        if (txt && txt != "") {
            var synth = speechSynthesis;
            var uttr = new SpeechSynthesisUtterance(txt);
            uttr.rate = 1.5;
            uttr.voice = synth.getVoices[4];
            uttr.pitch = 3;
            synth.speak(uttr);
        }
    } catch (e) {
       
    }

}