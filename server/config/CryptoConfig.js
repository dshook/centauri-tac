export default class CryptoConfig
{
  constructor()
  {

    /**
     * Number of rounds (exponential) used during bcrypt
     */
    this.rounds = 0 | process.env.BCRYPT_ROUNDS || 10;
  }
}
