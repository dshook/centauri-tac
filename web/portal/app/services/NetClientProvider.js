import NetClient from 'net-client';

// TODO: get rid of this shit
const MASTER_URL = window.location.origin + '/components/master';

export default class NetClientProvider
{
  constructor()
  {

  }

  $get(httpTransport)
  {
    return new NetClient(MASTER_URL, httpTransport);
  }
}
