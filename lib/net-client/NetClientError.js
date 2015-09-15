/**
 * Problems when communicatoing via the net client
 */
export default class NetClientError extends Error
{
  constructor(message = '')
  {
    super();
    this.name = 'NetClientError';
    this.message = message;
    const e = new Error();
    e.name = this.name;
    e.message = this.message;
    this.stack = e.stack;
  }
}
