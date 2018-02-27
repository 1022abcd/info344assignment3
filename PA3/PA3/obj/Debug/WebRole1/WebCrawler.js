function go() {
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
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/StopCrawling",
        success: function (data) {
        }
    });
}

function clear2() {
    console.log("yay");
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/ClearIndex",
        success: function (data) {
        }
    });
}

function findTitle() {
    $("#title").empty();
    $.ajax({
        type: "POST",
        data: JSON.stringify({ 'link': $("#input").val().trim()}),
        dataType: "json",
        url: "Admin.asmx/GetPageTitle",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            div.innerHTML = data.d;
            $("#title").append(div);
        }
    });
}

function getHTMLQueueCount() {
    console.log("gethtml");
    $.ajax({
        type: "POST",
        data: {},
        dataType: "json",
        url: "Admin.asmx/GetHTMLQueueCount",
        success: function (data) {
            console.log(data);
            var test = stringify(data.d);
            var div = document.createElement("div");
            div.innerHTML = test;
            $("#htmlCount").append(div);
        }
    });
}

window.onload = function getHTMLQueueCount() {
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/GetHTMLQueueCount",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            div.innerHTML = data;
            $("#htmlCount").append(div);
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/GetLinkQueueCount",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            div.innerHTML = data;
            $("#xmlCount").append(div);
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/GetState",
        success: function (data) {   
            console.log(data);
            data = data.replace('[', '');
            data = data.replace(']', '');
            var div = document.createElement("div");
            div.innerHTML = data;
            $("#state").append(div);
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/GetPerformance",
        success: function (data) {
            console.log(data);
            data = data.replace('[', '');
            data = data.replace(']', '');
            var split = data.split(',');
            
            var memory = document.createElement("div");
            var cpu = document.createElement("div");
            cpu.innerHTML = split[0];
            memory.innerHTML = split[1];
            $("#memory").append(memory);
            $("#cpu").append(cpu)
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/LastTenTable",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            data = data.replace('[', '');
            data = data.replace(']', '');
            div.innerHTML = data;
            $("#lastten").append(div);
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "json",
        url: "Admin.asmx/GetErrors",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            div.innerHTML = data;
            $("#error").append(div);
        }
    });
}