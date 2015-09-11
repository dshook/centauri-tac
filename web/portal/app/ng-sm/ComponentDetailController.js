export default class ComponentDetailController
{
  constructor(netClient, $stateParams)
  {
    this.id = $stateParams.id;
    this.net = netClient;
  }
}
