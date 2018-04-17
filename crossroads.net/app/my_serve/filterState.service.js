"use strict()";
(function(){

  module.exports = FilterState;

  function FilterState(){

    var filterState =  {
      memberIds: [],
      times: [],
      teams:[],
      signUps:[],
      addFamilyMember: function (memberId) {
        filterState.memberIds.push(memberId);
      },
      addSignUp: function (signUp) {
        filterState.signUps.push(signUp);
      },
      addTeam: function (team) {
       filterState.teams.push(team);
      },
      addTime: function (time) {
        filterState.times.push(time);
      }, 
      clearAll: function () {
        filterState.memberIds = [];
        filterState.signUps = [];
        filterState.times = [];
        filterState.teams = [];
      },
      findMember: function(memberId){
        return _.find(filterState.memberIds, function(m){
          return memberId === m;
        });
      },
      findSignUp: function(signUp) {
        return _.find(filterState.signUps, function(s){
          return signUp === s;
        });
      },
      findTeam: function(team){
        return _.find(filterState.teams, function(t){
          return team === t;
        });
      },
      findTime: function(time){
        return _.find(filterState.times, function(t){
          return time === t;
        });
      },
      isActive: function(){
        return filterState.memberIds.length > 0 || 
          filterState.times.length > 0 ||
          filterState.teams.length > 0 ||
          filterState.signUps.length > 0; 
      },
      removeFamilyMember: function(memberId){
        filterState.memberIds = _.filter(filterState.memberIds,function(m){
          return m !== memberId;
        });
      },
      removeSignUp: function(signUp) {
        filterState.signUps = _.filter(filterState.signUps, function(s) {
          return s !== signUp
        })
      },
      removeTeam: function(team) {
        filterState.teams = _.filter(filterState.teams,function(t){
          return t !== team
        });
      },
      removeTime: function(time) {
        filterState.times = _.filter(filterState.times,function(t){
          return t !== time
        });
      },
      getFamilyMembers: function(){
        return filterState.memberIds;
      } ,
      getSignUps: function() {
        return filterState.signUps;
      },
      getTeams: function() {
        return filterState.teams;
      },
      getTimes: function(){
        return filterState.times;
      }
    };
    return filterState;
  }

})();
