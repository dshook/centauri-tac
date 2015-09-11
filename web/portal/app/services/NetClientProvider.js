import NetClient from 'net-client';

// TODO: get rid of this shit
const MASTER_URL = 'http://localhost:10123/components/master';

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
