import Component from 'models/Component';

const base = 'http://localhost:10123/components/master/rest/';

export default class ComponentService
{
  constructor($resource)
  {
    this._Component = $resource(base + 'component/:id');
  }

  async getComponents()
  {
    const req = this._Component.query({});
    await req.$promise;

    return req.map(x => Component.fromJSON(x));
  }
}
