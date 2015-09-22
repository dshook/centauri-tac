/**
 * Wrap an async function in a $scope.$apply call to ensure the UI is updated
 * when things happen in async land
 */
export default function angularApplyDecorator(target, name, desc)
{
  const _f = desc.value;

  desc.value = function wrappedAsyncFunction() {

    if (!this.$scope) {
      throw new Error('must have a $scope as an instance member when using @ngApply');
    }

    // Converts a call into async!!!
    const ret = Promise.resolve(_f.apply(this, arguments));

    // This ensures that the apply calls happens after the stack frame that
    // called the function resolves. A little hacky but allows this decorator
    // to fix issues with libraries that are expecting anuglar to wrap their
    // calls for them
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


