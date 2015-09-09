export default function angularApplyDecorator(target, name, desc)
{
  const _f = desc.value;

  desc.value = function wrappedAsyncFunction() {

    if (!this.$scope) {
      throw new Error('must have a $scope as an instance member when using @ngApply');
    }

    const ret = _f.apply(this, arguments);

    const _apply = () => setTimeout(() => this.$scope.$apply(), 0);

    return ret
      .then((val) => {
        _apply();
        return val;
      })
      .catch((err) => {
        _apply();
        throw err;
      },
    );
  };
}


