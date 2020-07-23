angular.module("umbraco").controller("Tinifier.Test.Controller",
    function ($scope, $http, $injector, umbRequestHelper, editorService, stylesheetResource, notificationsService) {

        // var url = umbRequestHelper.getApiUrl('tinifierApiRoot', 'TestController');
        // console.log(url);
        var count = 0;

        var vm = this;
        vm.checked = false;
        vm.disabled = false;

        vm.toggle = toggle;

        $scope.message = "hello";

        function toggle() {
            vm.checked = !vm.checked;

            
        };


        $scope.click = function click() {
            $scope.checked = !$scope.checked;
            $scope.message = "event " + count++ + "   vm.checked: " + $scope.checked;
            console.log("event");
        };




        $('#btn1').on('click', function () {
            $('#container').empty();
            $('#container').append("<p>Waiting for response ...</p>");

            //$http.get(`../umbraco/api/Test`).then(successCallback, errorCallback);
            $http.get(`../umbraco/backoffice/api/Tinifier/Test`).then(successCallback, errorCallback);




            //vm.submit = submit;


          // var options = {
          //     title: "The confirmation",
          //     view: "/App_Plugins/Tinifier/Backoffice/Dashboards/TinifyAll.html"
          // };
          //
          // editorService.open(options);






            var result = "";
            var padding = 0;

            function successCallback(response) {
                $('#container').empty();
                console.log("Success: ");
                console.log(response);

                showData(response);
                $('#container').append("<p style=\"color:green\">Success</p>");
                $('#container').append("<p style=\"border-left: 6px solid green; background-color: lightgrey; padding: 5px;\">"
                    + response.statusText + "</p>");
                $('#container').append(result);
                console.log(response);

                //notificationsService.success("Success", response.message);

                notificationsService.add({
                    headline: 'Custom notification',
                    message: 'My custom notification message <a href="https://www.w3schools.com/html/">Visit our HTML tutorial</a>',
                    url: 'http://www.colours.nl',
                    type: 'Success'
                });        


                //exploreData(response.data);
            }

            function errorCallback(error) {
                $('#container').empty();
                $('#container').append("<p style=\"color:red\">Error</p>");
                $('#container').append("<p style=\"border-left: 6px solid red; background-color: lightgrey; padding: 5px;\">" + JSON.stringify(error.data) + "</p>");
            }

            function showData(input) {

                if (input instanceof Array) {
                    for (var i = 0; i < input.length; i++) {
                        showData(input[i]);
                    }
                } else {
                    for (var key in input) {
                        if (input[key] instanceof Object) {
                            padding = padding + 10;
                            result = result + "<div style=\"padding-left: " + padding + "px; border-left: 1px solid green;\" >";
                            result = result + "<p style=\"padding-top: 15px; \"><strong>" + key + '</strong></p>';
                            showData(input[key]);
                            result = result + "</div>";
                            padding = padding - 10;

                        } else {
                            result = result + "<p>" + key + ': ' + input[key] + "</p>";
                        }
                    }
                }
            }
        });
    });