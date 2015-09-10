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
    this.stack = (new Error()).stack;
  }
}
