function pageLoad() {
    DoNoti();

    //
}


function DoNoti() {
    if(allownoti){
        if (Notification.permission == "granted") {

            var nbody = "<div>" + notibody.field + "</div><div>Time:" + notibody.time + "</div><div>Will Spawn in 5 Minutes</div>";
            
            var noti = new Notification(notibody.field + " Saint Will Spawn in 5 minutes", [
                body = nbody, icon = "images/vengeful.png", timestamp = notibody.time, badge = "images/vengeful.png"
            ]);

        } else {
            Notification.requestPermission();
        }
        allownoti = false;
    }

}