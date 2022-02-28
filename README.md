## Simple ElasticSearch Alerting by Redev

Sear is a bare bones application to provide simple alerting for ElasticSearch. It simply queries your ES data, as defined in an Alert config, when the query hits are higher or lower than the specified threshold an action is performed.

![Example Teams Alert](https://raw.githubusercontent.com/RedevLtd/Sear/master/TeamsAlert.PNG)

## Installation

### Docker

	docker run -v /home/sear/alerts/:/app/Alerts -p 8080:8080 cadab/sear

## Example Configuration

Below is an example Sear Alert config, the Query is sent to your ES server as define.

If the number of hits falls below 100 then the action will be triggered based on the escalation timespan.

For example a timespan of `0` would send the action immediately, a timespan of `60` would only send once the alert has been triggered for over an hour. This allows you to create different actions based on the severity and length of an issue.

The interval is set via a standard cron definition.

For most alerts you can define the 'SimpleQuery' object, this has properties for the search text of your query and the timespan to look back for. If this is defined then the `AdvancedQuery` will be ignored, you only need one defined unlike the example.

The `AdvancedQuery` allows you to define a more complex ElasticSearch query. In the example below both the simple and advanced queries would return the same results.

	{
	    "Name": "Test Alert",
	    "Interval": "* * * * *",
	    "Host": "http://localhost:9200",
	    "Index": "logstash-*",
	    "AdvancedQuery": "{\"query\":{\"bool\":{\"must\":[{\"query_string\":{\"query\":\"MessageObject.MetricType: \\\"SyncStats\\\"\",\"analyze_wildcard\":true,\"default_field\":\"*\"}},{\"query_string\":{\"query\":\"*\",\"analyze_wildcard\":true,\"default_field\":\"*\"}},{\"match_phrase\":{\"MessageObject.MetricType\":{\"query\":\"SyncStats\"}}},{\"range\":{\"@timestamp\":{\"gte\":\"now-30m\",\"lte\":\"now\"}}}],\"filter\":[],\"should\":[],\"must_not\":[]}}}",
	    "SimpleQuery": {
		"SearchQuery": "MessageObject.MetricType: \\\"SyncStats\\\"",
		"TimeSpan": "now-30m"
	    },
	    "HitType": "Lower",
	    "Hits": 100,
	    "Actions": [
		{
		    "$type": "SearAlertingServiceCore.Actions.SendGridEmailAction, SearAlertingServiceCore",
		    "SendGridApiKey": "<SendGridEmailAccessToken>",
		    "To": "info@redev.co.uk",
		    "From": "info@redev.co.uk",
		    "EscalationTimeSpan": 0
		},        
		{
		    "$type": "SearAlertingServiceCore.Actions.SlackAction, SearAlertingServiceCore",
		    "MessagePrefix": "<@here>",
		    "SlackUrl": "https://hooks.slack.com/services/<SlackHookToken>",
		    "EscalationTimeSpan": 0,
		    "Link": "http://linktoyourviz.co.uk"
		}
		{
		    "$type": "SearAlertingServiceCore.Actions.SlackAction, SearAlertingServiceCore",
		    "MessagePrefix": "<@everyone> Alert has been triggered for over an hour!",
		    "SlackUrl": "https://hooks.slack.com/services/<SlackHookToken>",
		    "EscalationTimeSpan": 60,
		    "Link": "http://linktoyourviz.co.uk"
		}

	    ],    
	    "HasTriggered": false,
	    "WhenTriggered": null,
	    "AlertOnImproved": false
	}

### Actions

Multiple actions can be defined for an alert. Currently Slack, Teams, Email (via SendGrid) and SMS (via AQL) actions are supported, below are example configs of each.

`EscalationTimeSpan` This defines how many minutes SEAR will wait since the alert first triggered before firing this action. e.g. a value of `60` would only fire the action if the alert had been triggering continuously for over an hour. `0` would fire the action immediately.

#### Slack

    {
        "$type": "SearAlertingServiceCore.Actions.SlackAction, SearAlertingServiceCore",
        "MessagePrefix": "<@everyone>",
        "SlackUrl": "https://hooks.slack.com/services/<SlackHookToken>",
        "EscalationTimeSpan": 0,
        "Link": "http://linktoyourviz.co.uk"
     }

#### Teams

    {
        "$type": "SearAlertingServiceCore.Actions.TeamsAction, SearAlertingServiceCore",        
        "TeamsUrl": "<TeamsIncomingWebhookUrl>",
        "EscalationTimeSpan": 0,
        "Link": "http://linktoyourviz.co.uk"
     }

#### Email (SendGrid)

Multiple `To` emails can be defined, seperated with a semi-colon

    {
		"$type": "SearAlertingServiceCore.Actions.SendGridEmailAction, SearAlertingServiceCore",
		"SendGridApiKey": "<SendGridEmailAccessToken>",
		"To": "info@redev.co.uk;support@redev.co.uk",
		"From": "info@redev.co.uk",
		"EscalationTimeSpan": 0
    }

#### SMS (AQL)

Multiple `Numbers` can be defined

    {
        "$type": "SearAlertingServiceCore.Actions.AqlSmsAction, SearAlertingServiceCore",
        "AqlUrl": "https://api.aql.com/v2/sms/send",
        "AqlToken": "<AqlSMSToken>",
        "Numbers": [ "447123456789", "447987654321" ],
        "EscalationTimeSpan": 0
    }

### Todo

- [x] Alert on resolved/improved
- [x] Email Alert Action
- [x] Web Dashboard
- [x] Specific configs for alert actions
- [x] Alert Escalation actions
- [ ] Add/Remove/Modify alert rules while running
- [ ] Spike trigger type
- [ ] Prevent alert spam (multiple on/off trigger messages in short time window)
- [ ] Better docs :)
