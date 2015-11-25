﻿app.service('TutorService', ["$http", "$q", "HttpRequest", function ($http, $q, HttpRequest) {
    this.getWords = function (id) {
        var deferred = $q.defer();
        HttpRequest.get("api/TrainingWordSuite/NotStudiedWords?id=" + id, deferred);
        return deferred.promise;
    }
}]);