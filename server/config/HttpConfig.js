/**
 * Configuration of the HTTP server
 */
export default class HttpConfig
{
  constructor()
  {

    /**
     * Port to listen for HTTP connections on
     */
    this.port = process.env.PORT || 10123;

    /**
     * Public-facing URL
     */
    this.publicURL = process.env.PUBLIC_URL || 'http://localhost:' + this.port;
  }
}
