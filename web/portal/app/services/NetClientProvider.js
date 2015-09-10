import NetClient from 'net-client';

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
