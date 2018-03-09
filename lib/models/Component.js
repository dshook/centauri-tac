export default class Component
{
  constructor()
  {
    this.type = null;
    this.isActive = null;

    // express server on server
    this.httpServer = null;

    // http harness for rest controllers on server
    this.restServer = null;

    // socket server (actually the harness) for socket shit on server
    this.sockServer = null;

    // socket client when on client
    this.sockClient = null;

    // container to register stuff on
    this.container = null;
  }
}
