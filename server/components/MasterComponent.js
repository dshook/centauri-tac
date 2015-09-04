import loglevel from 'loglevel-decorator';
import ComponentAPI from '../api/ComponentAPI.js';
import ComponentTypeAPI from '../api/ComponentTypeAPI.js';

// import Component from 'models/Component';

@loglevel
export default class MasterComponent
{
  constructor(rest)
  {
    this.rest = rest;
  }

  async start()
  {
    this.rest.mountController('/component/type', ComponentTypeAPI);
    this.rest.mountController('/component', ComponentAPI);
  }
}
