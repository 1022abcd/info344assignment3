function go() {
    crawling = true;
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/StartCrawling",
        success: function (data) {
        }
    });
}

function stop() {
    crawling = true;
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/StopCrawling",
        success: function (data) {
        }
    });
}

function clear() {
    crawling = true;
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/ClearIndex",
        success: function (data) {
        }
    });
}
