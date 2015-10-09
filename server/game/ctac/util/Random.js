export default class Random{
  static Range(maximum, minimum){
    return Math.floor(Math.random() * (maximum - minimum + 1)) + minimum;
  }
}