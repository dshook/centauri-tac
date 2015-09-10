import HttpTransport from 'http-transport';

/**
 * Angular server provider for the HTTP trasnport instance
 */
export default class HttpTransportProvider
{
  constructor()
  {

  }

  $get()
  {
    return new HttpTransport();
  }
}
