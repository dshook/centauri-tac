import loglevel from 'loglevel-decorator';
import ComponentAPI from '../api/ComponentAPI.js';
import ComponentTypeAPI from '../api/ComponentTypeAPI.js';

@loglevel
export default class MasterComponent
{
  async start(server, rest)
  {
    rest.mountController('/component/type', ComponentTypeAPI);
    rest.mountController('/component', ComponentAPI);
  }
}
