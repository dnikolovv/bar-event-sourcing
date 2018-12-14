((app, signalR) => {
  app.factory("SignalRService", function() {
    const connection = new signalR.HubConnectionBuilder().withUrl("/eventsHub").build();

    connection.start().catch(function(error) {
      console.log("Failed to start SignalR connection.");
      console.error(error);
    });

    return {
      subscribeToNewEvent: subscribeToNewEvent
    }

    function subscribeToNewEvent(handler) {
      connection.on("NewEventRegistered", handler);
    }
  });
})(app, signalR);