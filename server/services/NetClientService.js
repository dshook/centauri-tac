import NetClient from 'net-client';

/**
 * Net client (used for component communication on the server)
 */
export default class NetClientService
{
  constructor(app, componentsConfig, httpTransport, auth)
  {
    const net = new NetClient(
        componentsConfig.masterURL,
        componentsConfig.realm,
        httpTransport);

    // Create a long-lasting token to use for backend RPC communication
    const token = auth.generateToken(null, ['component'], 60 * 24 * 365);

    net.token = token;

    app.registerInstance('netClient', net);
  }
}
