(app => {
  app.factory("TabService", ["$http", "toastr", function ($http, toastr) {

    const baseUrl = "api/tab";

    return {
      openTab: openTab,
      closeTab: closeTab,
      orderBeverages: orderBeverages,
      serveBeverages: serveBeverages,
      getTabDetails: getTabDetails
    };

    function openTab(tabId, clientName, success) {
      return $http.post(`${baseUrl}/open/${tabId}`, `"${clientName}"`)
        .then(success, showResponseErrors);
    }

    function closeTab(tabId, amountPaid, success) {
      return $http.post(`${baseUrl}/close/${tabId}`, amountPaid)
        .then(success, showResponseErrors);
    }

    function orderBeverages(tabId, beverageNumbers, success) {
      return $http.post(`${baseUrl}/order/${tabId}`, beverageNumbers)
        .then(success, showResponseErrors);
    }

    function serveBeverages(tabId, beverageNumbers, success) {
      return $http.post(`${baseUrl}/serve/${tabId}`, beverageNumbers)
        .then(success, showResponseErrors);
    }

    function getTabDetails(tabId, success) {
      return $http.get(`${baseUrl}/${tabId}`)
        .then(response => success(response.data), showResponseErrors);
    }

    function showResponseErrors(response) {
        const errors = response.data.messages;

        if (errors) {
            toastr.error(errors.join("; ", "Error"));
        }
    }
  }]);
})(app);