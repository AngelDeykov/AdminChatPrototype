function openInNewWindowWithSettings() {
    var url = $('#chatUrl').val();
    window.open(url, "Live Chat Support", "height=500,width=550,location=no");
}

$(function () {
    $(".flip").flip({
        trigger: 'hover',
        axis: 'x'
    });
});
