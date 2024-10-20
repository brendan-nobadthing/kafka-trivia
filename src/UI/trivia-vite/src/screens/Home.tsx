import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs"
 
export function Home() {
  return (
    <>
    <h1>Fast Trivia</h1>
    <Tabs defaultValue="join" className="w-[400px]">
      <TabsList className="grid w-full grid-cols-2">
        <TabsTrigger value="join">Join Game</TabsTrigger>
        <TabsTrigger value="password">New Game</TabsTrigger>
      </TabsList>
      <TabsContent value="join">
        <Card>
          <CardHeader>
            <CardTitle>Join Game</CardTitle>
            <CardDescription>
              Enter Game Code and your details to join. <br/>
              Email address is not shared - for gravitar pic only
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
          <div className="space-y-1">
              <Label htmlFor="code">Game Code</Label>
              <Input id="code" defaultValue="" />
            </div>
            <div className="space-y-1">
              <Label htmlFor="name">Display Name</Label>
              <Input id="name" defaultValue="" />
            </div>
            <div className="space-y-1">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" defaultValue="" />
            </div>
          </CardContent>
          <CardFooter>
            <Button>Join Game</Button>
          </CardFooter>
        </Card>
      </TabsContent>
      <TabsContent value="password">
        <Card>
          <CardHeader>
            <CardTitle>Password</CardTitle>
            <CardDescription>
              Change your password here. After saving, you'll be logged out.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="space-y-1">
              <Label htmlFor="current">Current password</Label>
              <Input id="current" type="password" />
            </div>
            <div className="space-y-1">
              <Label htmlFor="new">New password</Label>
              <Input id="new" type="password" />
            </div>
          </CardContent>
          <CardFooter>
            <Button>Save password</Button>
          </CardFooter>
        </Card>
      </TabsContent>
    </Tabs>
    </>
    
  )
}
