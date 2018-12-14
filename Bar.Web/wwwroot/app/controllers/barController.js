(app => {
  app.controller("BarController", ["TabService", "SignalRService", "beverages", "$stateParams", "$state", "$scope", function (TabService, SignalRService, beverages, $stateParams, $state, $scope) {

    const vm = this;

    vm.beverages = beverages;
    vm.tab = $stateParams.tab;
    vm.isOutstanding = isOutstanding;
    vm.orderBeverage = orderBeverage;
    vm.serveBeverage = serveBeverage;
    vm.closeTab = closeTab;
    vm.events = [];

    SignalRService.subscribeToNewEvent(function (event) {
      $scope.$apply(function() {
        vm.events.push(event);
      });
    });

    function serveBeverage(menuNumber) {
      TabService.serveBeverages(vm.tab.id, [menuNumber], () => refreshTabDetails());
    }

    function closeTab(amountPaid) {
      TabService.closeTab(vm.tab.id, amountPaid, () => getCurrentTabDetails(tab => $state.go("tabClosed", { tab: tab })));
    }

    function orderBeverage(menuNumber) {
      TabService.orderBeverages(vm.tab.id, [menuNumber], () => refreshTabDetails());
    }

    function refreshTabDetails() {
      getCurrentTabDetails(tab => vm.tab = tab);
    }

    function getCurrentTabDetails(success) {
      return TabService.getTabDetails(vm.tab.id, success);
    }

    function isOutstanding(menuNumber) {
      return vm.tab.outstandingBeverages &&
             vm.tab.outstandingBeverages.some(b => b.menuNumber === menuNumber);
    }
  }]);
})(app);