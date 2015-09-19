import metaDecorator from 'meta-decorator';

export default
{
  command: metaDecorator('command'),
  connected: metaDecorator('connected'),
  disconnected: metaDecorator('disconnected'),
  middleware: metaDecorator('middleware'),
};

