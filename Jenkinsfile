pipeline {
    agent { label 'devworbldng04' }
    stages {
        stage('Running Code Analysis with Build') {
            steps {
                bat 'cd SecurityManager && dotnet build --configuration release'
            }
        }
        stage('Running Unit Tests') {
            steps {
                bat 'cd SecurityManager && dotnet test --configuration release --filter FullyQualifiedName!~IntegrationTests'
            }
        }
        stage("Publish Stage with Release Configuration") {
            steps {
                bat 'cd SecurityManager && dotnet publish --configuration release'
            }
        }
        stage ('Archive Artifacts') {
			steps {
               archiveArtifacts([ 
			                 allowEmptyArchive: false, 
			                 artifacts: '**/SecurityManager/bin/Release/net8.0/publish/**', 
			                 excludes: '**/*.pdb',
                             fingerprint: true, 
			                 followSymlinks: false, 
			                 onlyIfSuccessful: true
							])
			}
		}
    }
}