# info344assignment3

I used class library for: 
    -Cloud Storage
        :Table
        :Queue
    -Table Entities
        :Performance Counter
        :Error Message
        :HTML Page
    -Class 
        :Hash

I used queue and table to transfer data between webrole and workerrole.

WebRole
    -StartCrawling
        :Adding "cnn.com/robots.txt" & "bleacherreport.com/robots.txt" into the cloud queue
        to initiate.
    -StopCrawling
        :Adding "Stop" command to the commandQueue to stop crawling.
    -ClearIndex
        :Clears all the data including queues and tables.
    -GetLinkQueueCount
        :returns the number of links in the linkQueue.
    -GetHTMLQueueCount
        :returns the number of links in the htmlQueue.
    -GerPerformance
        :returns the performance counter of cpu and ram usage.
    -LastTenTable
        :returns the last ten table in the Cloud Table.
    -GetPageTitle
        :returns the title of the given url.
    -GetState
        :returns the current state.

WorkerRole
    -CrawlUrl
        :Changes the state to "Loading"
        :distinguishes the given url
            *robots.txt : HandleRobotstxt
            *-index.xml : CrawlSiteMapIndex
            *.xml : CrawlSiteMap
    -GetHTMLData
        :Changes the state to "Crawling"
        :get the title and the publication date of the url and send those to PageEntity.cs,
        then add that entity to the cloud table
        :get all the hrefs and add them to the queue
    -GetPerformanceCounter
        :returns the cpu and ram usage while running the project
        
        
        
1022abcd.cloudapp.net
https://github.com/1022abcd/info344assignment3