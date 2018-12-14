(app => {
  app.controller("TabClosedController", ["$state", "$stateParams", function ($state, $stateParams) {
    const vm = this;
    vm.tab = $stateParams.tab;
    vm.openTab = openTab;

    function openTab() {
      $state.go("openTab");
    }
  }]);
})(app);