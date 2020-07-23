angular.module("umbraco").controller("Tinifier.TinifierEditTSetting.Controller",
    function ($scope, $http, $injector, $routeParams, notificationsService, editorService, appState, eventsService) {
        // Get settings
        $scope.timage = {};

        // Fill select dropdown
        $scope.options = [
            { value: false, label: "False" },
            { value: true, label: "True" }
        ];

        function HideLeftPannel() {
            console.log("hideLeftPanel");
            var contentwrapperContainer = document.getElementById("contentwrapper");
            var navigationContainer = document.getElementById("navigation");
            appState.setGlobalState("showNavigation", false);
            contentwrapperContainer.style.left = "0px";
            navigationContainer.style.width = "0px";
        }

        function SetDefaultLeftPannel() {

            console.log("default settings");
            var element = document.getElementById("contentwrapper");
            defaultContentwrapperStyle = window.getComputedStyle(element, null).getPropertyValue("left");
            appState.setGlobalState("showNavigation", true);
            element.removeAttribute("style");

            navigationContainer = document.getElementById("navigation");
            navigationContainer.removeAttribute("style");
        }

        //HideLeftPannel();

        var subscriber = eventsService.on('appState.sectionState.changed', function (event, args) {

            if (!args.value)
                return;
            if (args.value != "tinifier") {

                SetDefaultLeftPannel();
                eventsService.unsubscribe(subscriber);
            }
        });

        // Fill form from web api
        $http.get(`/umbraco/backoffice/api/TinifierSettings/GetTSetting`).then(
            function (response) {
                $scope.timage = response.data;
                console.log($scope.timage.HideLeftPanel);
                if ($scope.timage.HideLeftPanel)
                    HideLeftPannel();
                else
                    SetDefaultLeftPannel();
            },
            function (error) {
                notificationsService.error(error.message);
            });


        $scope.submit = function () {

            SubmitSettings();

            //timage = $scope.timage;
            //timage.ApiKey = $('#apiKey').val();
            //timage.EnableOptimizationOnUpload = $('#enableOptimizationOnUpload :selected').text().toLowerCase() === "true";
            //timage.HideLeftPanel = $('#hideLeftPanel :selected').text().toLowerCase() === "true";
            //timage.PreserveMetadata = $('#preserveMetadata :selected').text().toLowerCase() === "true";
            //timage.EnableUndoOptimization = $('#enableUndoOptimization :selected').text().toLowerCase() === "true";
            //
            //$http.post(`/umbraco/backoffice/api/TinifierSettings/PostTSetting`, JSON.stringify(timage)).then(postSuccessCallback, postErrorCallback);
            //
            //function postSuccessCallback(response) {
            //    notificationsService.success("Success", response.message);
            //}
            //
            //function postErrorCallback(error) {
            //    if (error.Error === 1) {
            //        notificationsService.warning("Warning", error.message);
            //    }
            //    else {
            //        notificationsService.error("Error", error.data.headline + " " + error.data.message);
            //    }
            //}
        };

        $scope.stopTinifing = function () {
            var options = {
                title: "The confirmation",
                view: "/App_Plugins/Tinifier/Backoffice/Dashboards/StopTinifyingConfirmation.html"
            };

            editorService.open(options);

            //$http.delete(`/umbraco/backoffice/api/TinifierState/DeleteActiveState`).then(function (response) {
            //    notificationsService.success("Success", "Tinifing successfully stoped!");
            //}, function (response) {
            //    notificationsService.error("Error", "Tinifing can`t stop now!");
            //});
        };

        $scope.tinifyEverything = function () {
            var options = {
                title: "The confirmation",
                view: "/App_Plugins/Tinifier/Backoffice/Dashboards/TinifyAll.html"
            };

            editorService.open(options);
        };

        var processNotification = null;
        function SubmitSettings() {
            notificationsService.success("Saving in progress ...");

            console.log(notificationsService.current);
            processNotification = notificationsService.current[0];

            console.log(processNotification);
            // notificationsService.add({
            //     view: "/App_Plugins/Tinifier/BackOffice/Dashboards/Test.html",
            //     sticky: true,
            //     type: 'custom'
            // });

            timage = $scope.timage;
            timage.ApiKey = $('#apiKey').val();
            //timage.EnableOptimizationOnUpload = $('#enableOptimizationOnUpload :selected').text().toLowerCase() === "true";
            //timage.HideLeftPanel = $('#hideLeftPanel :selected').text().toLowerCase() === "true";
            //timage.PreserveMetadata = $('#preserveMetadata :selected').text().toLowerCase() === "true";
            //timage.EnableUndoOptimization = $('#enableUndoOptimization :selected').text().toLowerCase() === "true";
           


            $http.post(`/umbraco/backoffice/api/TinifierSettings/PostTSetting`, JSON.stringify(timage)).then(postSuccessCallback, postErrorCallback);

            function postSuccessCallback(response) {

                for (var i = 0; i < notificationsService.current.length; i++) {
                    notificationsService.remove(notificationsService.current[i]);
                };

                notificationsService.remove(processNotification);
                notificationsService.success("✔️ Settings successfully saved!");

                if ($scope.timage.HideLeftPanel)
                    HideLeftPannel();
                else
                    SetDefaultLeftPannel();
            }

            function postErrorCallback(error) {
                notificationsService.remove();

                if (error.Error === 1) {
                    notificationsService.warning("Warning", error.message);
                }
                else {
                    notificationsService.error("Error", error.data.headline + " " + error.data.message);
                }
            }
        }

        $(document).ready(function () {
            var previousApiKey = "";

            $("#apiKey").focusout(function () {
                ValidateApiKey();
            });

            $("#apiKey").focus(function () {
                previousApiKey = $('#apiKey').val();
            });

            $("#apiKey").keypress(function (event) {
                if (event.charCode == 13)
                    ValidateApiKey();
            });

            $scope.hideLeftPanelSetting = function () {
                $scope.timage.HideLeftPanel = !$scope.timage.HideLeftPanel;
                SubmitSettings();
            };

            $scope.preserveMetadataSetting = function () {
                $scope.timage.PreserveMetadata = !$scope.timage.PreserveMetadata;
                SubmitSettings();
            };

            $scope.enableUndoOptimizationSetting = function () {
                $scope.timage.EnableUndoOptimization = !$scope.timage.EnableUndoOptimization;
                SubmitSettings();
            };

            $scope.enableOptimizationOnUploadSetting = function () {
                $scope.timage.EnableOptimizationOnUpload = !$scope.timage.EnableOptimizationOnUpload;
                SubmitSettings();
            };

            function ValidateApiKey() {
                var actualApiKey = $('#apiKey').val();

                if (previousApiKey == null)
                    previousApiKey = "";

                if (previousApiKey !== actualApiKey)
                    SubmitSettings();

                previousApiKey = actualApiKey;
            }

        });
    });