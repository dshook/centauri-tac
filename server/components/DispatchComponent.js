import DispatchRPC from '../api/DispatchRPC.js';

export default class DispatchComponent
{
  async start(component)
  {
    const {sockServer} = component;

    sockServer.addHandler(DispatchRPC);
  }
}
