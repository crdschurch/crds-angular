(function () {
'use strict';

module.exports = CorkboardSession;

CorkboardSession.$inject = [];

function CorkboardSession() {
  var session = {};

  session.posts = [];

  return session;
};
})();
