import HttpTransport from 'http-transport';

/**
 * Provide a transport instance to get things over HTTP
 */
export default class HttpTransportService
{
  constructor(app)
  {
    app.registerSingleton('httpTransport', HttpTransport);
  }
}
