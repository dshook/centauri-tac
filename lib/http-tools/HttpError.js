/**
 * Error class thats more expressive for HTTP error codes
 */
export default class HttpError extends Error
{
  constructor(code = 500, message = '')
  {
    super();
    this.name = 'HttpError';
    this.message = message;
    this.stack = (new Error()).stack;
    this.statusCode = code;
  }
}

