
//Not as much entropy as a GUID but should be good enough for our purposes
export default function uniqueId(){
  return Math.floor(Math.random() * 1000000);
}