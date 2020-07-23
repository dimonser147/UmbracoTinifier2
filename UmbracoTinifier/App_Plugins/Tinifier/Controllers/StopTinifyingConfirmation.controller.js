angular.module("umbraco").controller("Tinifier.StopTinifyingConfirmation.Controller",
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
                    headline: "Stop tinifing started",
                    message: "",
                    url: '/umbraco/#/tinifier',
                    type: 'success'
                });

            $http.delete(`/umbraco/backoffice/api/TinifierState/DeleteActiveState`).then(function (response) {
                notificationsService.success("Success", "Tinifing successfully stoped!");
                location.reload();
            }, function (response) {
                notificationsService.error("Error", "Tinifing can`t stop now!");
            });

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