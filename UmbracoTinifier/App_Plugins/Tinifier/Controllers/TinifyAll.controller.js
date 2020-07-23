angular.module("umbraco").controller("Tinifier.TinifyAll.Controller",
    function ($scope, $http, $injector, notificationsService, editorService) {


        var vm = this;

        vm.close = close;
        vm.submit = submit;
        vm.buttonState = "init";
        vm.clickButton = clickButton;


        function close() {
            editorService.close();
        }

        function submit() {

            notificationsService
                .add({
                    headline: "Tinifing started",
                    message: "click <a href=\"/umbraco/#/tinifier\" target=\"_blank\">here</a> for more details",
                    url: '/umbraco/#/tinifier',
                    type: 'success'
                });

            $http.put(`/umbraco/backoffice/api/Tinifier/TinifyEverything`).then(postSuccessCallback, postErrorCallback);


            function postSuccessCallback(response) {
                notificationsService.success("Success", response.message);
            }

            function postErrorCallback(error) {
                if (error.Error === 1) {
                    notificationsService.warning("Warning", error.message);
                }
                else {
                    notificationsService.error("Error", error.data.headline + " " + error.data.message);
                }
            }
            //
            // notificationsService.success("Start operation");
            editorService.close();
        }

        function clickButton() {

            vm.buttonState = "busy";

            setTimeout(function () { vm.buttonState = "success"; }, 1);
            // vm.buttonState = "success";



        }




    });