#!/usr/bin/env bash

if [ ! -n "$APP_CENTER_KEY" ]
then
    echo "You need define the $APP_CENTER_KEY variable in App Center"
    exit
fi

APP_CONSTANT_FILE=$APPCENTER_SOURCE_DIRECTORY/RPS/AppConstant.cs

if [ -e "$APP_CONSTANT_FILE" ]
then
    echo "Updating ApiUrl to $APP_CENTER_KEY in AppConstant.cs"
    sed -i '' 's#AppCenterKey = "[a-z:./]*"#AppCenterKey = "'$APP_CENTER_KEY'"#' $APP_CONSTANT_FILE

    echo "File content:"
    cat $APP_CONSTANT_FILE
fi