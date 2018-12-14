(app => {
  app.controller("OpenTabController", ["TabService", "$state", function (TabService, $state) {

    const vm = this;

    vm.openTab = function (clientName) {
      const newTabId = guid();
      TabService.openTab(newTabId, clientName, () => $state.go("bar", { tab: { id: newTabId, clientName: clientName } }));
    }
  }]);

  function guid() {
    function s4() {
      return Math.floor((1 + Math.random()) * 0x10000)
        .toString(16)
        .substring(1);
    }
    return s4() + s4() + "-" + s4() + "-" + s4() + "-" + s4() + "-" + s4() + s4() + s4();
  }
})(app);